from flask import Flask, jsonify, request
from flask_cors import CORS
import json
import os
import random
from models import get_db_connection, init_db

app = Flask(__name__)
CORS(app)

# 日本語をエスケープせずにUTF-8で返す.
app.config['JSON_AS_ASCII'] = False

# サーバー起動時にDB初期化.
try:
    init_db()
except Exception as e:
    print(f"DB init failed: {e}")

JSON_DATA_PATH = "/app/json_data"


@app.route('/api/health', methods=['GET'])
def health_check():
    """ヘルスチェック用エンドポイント."""
    return jsonify({"status": "ok", "message": "Server is running"})


@app.route('/api/data/<data_type>', methods=['GET'])
def get_data(data_type):
    """指定されたデータタイプに応じたJSONデータを返す."""
    file_mapping = {
        "test": "test_data.json",
        "player": "player_data.json",
        "game": "game_data.json",
        "config": "config_data.json"
    }

    if data_type not in file_mapping:
        return jsonify({"error": f"Unknown data type: {data_type}"}), 400

    file_path = os.path.join(JSON_DATA_PATH, file_mapping[data_type])

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        return jsonify(data)
    except FileNotFoundError:
        return jsonify({"error": f"Data file not found: {data_type}"}), 404
    except json.JSONDecodeError:
        return jsonify({"error": f"Invalid JSON in file: {data_type}"}), 500


@app.route('/api/data', methods=['POST'])
def receive_data():
    """Unityからのデータを受信する."""
    data = request.get_json()
    if data is None:
        return jsonify({"error": "No JSON data received"}), 400

    print(f"Received data: {data}")
    return jsonify({"status": "received", "data": data})


# デフォルトのレアリティ重み.
DEFAULT_RARITY_WEIGHTS = {
    1: 40.0,
    2: 30.0,
    3: 15.0,
    4: 10.0,
    5: 4.0,
    6: 1.0
}


@app.route('/api/gacha/weights', methods=['GET'])
def get_gacha_weights():
    """レアリティごとの重みを返す."""
    return jsonify({"weights": DEFAULT_RARITY_WEIGHTS})


@app.route('/api/gacha/characters', methods=['GET'])
def get_gacha_characters():
    """ガチャ対象キャラクターリストを返す."""
    try:
        conn = get_db_connection()
        cursor = conn.cursor(dictionary=True)
        cursor.execute("""
            SELECT masternumber, rarity, name, type
            FROM character_master
            WHERE type = 0
            ORDER BY rarity, masternumber
        """)
        characters = cursor.fetchall()
        cursor.close()
        conn.close()
        return jsonify({"characters": characters})
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/gacha/pull', methods=['POST'])
def pull_gacha():
    """ガチャを引く (サーバー側で重みづけ計算)."""
    data = request.get_json() or {}
    count = data.get('count', 10)

    try:
        conn = get_db_connection()
        cursor = conn.cursor(dictionary=True)

        # 恒常キャラクターを取得.
        cursor.execute("""
            SELECT masternumber, rarity, name, type
            FROM character_master
            WHERE type = 0
        """)
        characters = cursor.fetchall()
        cursor.close()
        conn.close()

        if not characters:
            return jsonify({"error": "No characters available"}), 400

        # レアリティごとにキャラをグループ化.
        rarity_groups = {}
        for char in characters:
            rarity = char['rarity']
            if rarity not in rarity_groups:
                rarity_groups[rarity] = []
            rarity_groups[rarity].append(char)

        # 重みの合計.
        total_weight = sum(DEFAULT_RARITY_WEIGHTS.values())

        results = []
        for _ in range(count):
            # レアリティを選択.
            rand_val = random.uniform(0, total_weight)
            current_weight = 0
            selected_rarity = 1

            for rarity in sorted(DEFAULT_RARITY_WEIGHTS.keys()):
                current_weight += DEFAULT_RARITY_WEIGHTS[rarity]
                if rand_val <= current_weight:
                    selected_rarity = rarity
                    break

            # そのレアリティのキャラから抽選.
            if selected_rarity in rarity_groups and rarity_groups[selected_rarity]:
                selected_char = random.choice(rarity_groups[selected_rarity])
                results.append(selected_char)

        return jsonify({"results": results, "weights": DEFAULT_RARITY_WEIGHTS})
    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
