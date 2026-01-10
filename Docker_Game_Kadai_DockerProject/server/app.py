# サーバーメインアプリケーション.
from flask import Flask, jsonify, request
from flask_cors import CORS
import json
import os

app = Flask(__name__)
CORS(app)

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


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
