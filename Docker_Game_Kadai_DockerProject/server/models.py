# データベースモデル定義（MySQL版）.
import mysql.connector
from mysql.connector import pooling
import os

DB_CONFIG = {
    'host': os.environ.get('DB_HOST', 'localhost'),
    'user': os.environ.get('DB_USER', 'game_user'),
    'password': os.environ.get('DB_PASSWORD', 'game_password'),
    'database': os.environ.get('DB_NAME', 'game_db'),
    'charset': 'utf8mb4',
    'collation': 'utf8mb4_unicode_ci',
}

# コネクションプール作成.
connection_pool = None


def init_pool():
    """コネクションプール初期化."""
    global connection_pool
    try:
        connection_pool = pooling.MySQLConnectionPool(
            pool_name="game_pool",
            pool_size=5,
            pool_reset_session=True,
            **DB_CONFIG
        )
    except mysql.connector.Error as err:
        print(f"DB connection pool error: {err}")
        raise


def get_db_connection():
    """データベース接続を取得."""
    global connection_pool
    if connection_pool is None:
        init_pool()
    return connection_pool.get_connection()


def init_db():
    """データベース初期化（テーブルはinit.sqlで作成済み）."""
    try:
        init_pool()
        print("Database connection pool initialized.")
    except Exception as e:
        print(f"Database initialization error: {e}")
