from flask import Flask, jsonify, request, send_from_directory, send_file
from flask_cors import CORS
import json
import os
import random
import io
from PIL import Image, ImageDraw, ImageFont
from models import get_db_connection, init_db

app = Flask(__name__)
CORS(app)

# 画像フォルダのパス.
IMAGES_PATH = "/app/images"

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
    0: 40.0,
    1: 30.0,
    2: 15.0,
    3: 10.0,
    4: 4.0,
    5: 1.0
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


@app.route('/api/images/rarity/<int:rarity>', methods=['GET'])
def get_rarity_image(rarity):
    """レアリティに対応する画像を返す."""
    if rarity < 0 or rarity > 5:
        return jsonify({"error": "Invalid rarity"}), 400

    image_filename = f"rarity_{rarity}.png"
    image_path = os.path.join(IMAGES_PATH, "rarity", image_filename)

    if os.path.exists(image_path):
        return send_from_directory(os.path.join(IMAGES_PATH, "rarity"), image_filename)
    else:
        # 画像が存在しない場合はプレースホルダーを生成.
        return generate_rarity_placeholder(rarity)


def generate_rarity_placeholder(rarity):
    """レアリティに対応するプレースホルダー画像を生成する."""
    # レアリティごとの色設定 (0～5).
    rarity_colors = {
        0: (128, 128, 128),   # グレー.
        1: (0, 128, 0),       # 緑.
        2: (0, 100, 255),     # 青.
        3: (128, 0, 128),     # 紫.
        4: (255, 215, 0),     # 金.
        5: (255, 100, 100),   # 赤.
    }

    color = rarity_colors.get(rarity, (128, 128, 128))

    # 画像サイズ.
    width, height = 100, 100
    img = Image.new('RGB', (width, height), color)
    draw = ImageDraw.Draw(img)

    # 星を描画 (レアリティ+1個の星を表示).
    star_text = "★" * (rarity + 1)
    try:
        font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 16)
    except:
        font = ImageFont.load_default()

    # テキストを中央に配置.
    bbox = draw.textbbox((0, 0), star_text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    x = (width - text_width) // 2
    y = (height - text_height) // 2
    draw.text((x, y), star_text, fill=(255, 255, 255), font=font)

    # バイトストリームに保存.
    img_io = io.BytesIO()
    img.save(img_io, 'PNG')
    img_io.seek(0)

    return send_file(img_io, mimetype='image/png')


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
