-- ゲームデータベース初期化SQL.
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

USE game_db;

-- キャラクタマスターデータテーブル.
CREATE TABLE IF NOT EXISTS character_master (
    masternumber INT AUTO_INCREMENT PRIMARY KEY,
    rarity INT NOT NULL CHECK(rarity >= 0 AND rarity <= 5),
    name VARCHAR(255) NOT NULL,
    type INT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- type: 0=恒常, 1=限定, 2~=その他.

-- キャラクタステータスマスターデータテーブル.
CREATE TABLE IF NOT EXISTS character_status_master (
    id INT AUTO_INCREMENT PRIMARY KEY,
    masternumber INT NOT NULL UNIQUE,
    hp_base FLOAT NOT NULL DEFAULT 100.0,
    mp_base FLOAT NOT NULL DEFAULT 50.0,
    str_base FLOAT NOT NULL DEFAULT 10.0,
    int_base FLOAT NOT NULL DEFAULT 10.0,
    vit_base FLOAT NOT NULL DEFAULT 10.0,
    res_base FLOAT NOT NULL DEFAULT 10.0,
    dex_base FLOAT NOT NULL DEFAULT 10.0,
    agi_base FLOAT NOT NULL DEFAULT 10.0,
    luk_base FLOAT NOT NULL DEFAULT 10.0,
    hp_rate FLOAT NOT NULL DEFAULT 1.0,
    mp_rate FLOAT NOT NULL DEFAULT 1.0,
    str_rate FLOAT NOT NULL DEFAULT 1.0,
    int_rate FLOAT NOT NULL DEFAULT 1.0,
    vit_rate FLOAT NOT NULL DEFAULT 1.0,
    res_rate FLOAT NOT NULL DEFAULT 1.0,
    dex_rate FLOAT NOT NULL DEFAULT 1.0,
    agi_rate FLOAT NOT NULL DEFAULT 1.0,
    luk_rate FLOAT NOT NULL DEFAULT 1.0,
    FOREIGN KEY (masternumber) REFERENCES character_master(masternumber) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ガチャ種類テーブル.
CREATE TABLE IF NOT EXISTS gacha_type (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    cost_type VARCHAR(50) NOT NULL,
    cost_amount INT NOT NULL,
    include_permanent TINYINT NOT NULL DEFAULT 1,
    pickup_list TEXT DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- pickup_list: JSON形式のmasternumberリスト.

-- ガチャ確率設定テーブル.
CREATE TABLE IF NOT EXISTS gacha_rate_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    gacha_id INT NOT NULL,
    rarity INT NOT NULL,
    weight FLOAT NOT NULL DEFAULT 1.0,
    pickup_weight FLOAT NOT NULL DEFAULT 1.0,
    permanent_weight FLOAT NOT NULL DEFAULT 1.0,
    FOREIGN KEY (gacha_id) REFERENCES gacha_type(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- テスト用キャラクターデータ (レアリティ0-5、各10体、全て恒常).
INSERT INTO character_master (rarity, name, type) VALUES
(0, 'レアリティ0 : 001番', 0), (0, 'レアリティ0 : 002番', 0), (0, 'レアリティ0 : 003番', 0),
(0, 'レアリティ0 : 004番', 0), (0, 'レアリティ0 : 005番', 0), (0, 'レアリティ0 : 006番', 0),
(0, 'レアリティ0 : 007番', 0), (0, 'レアリティ0 : 008番', 0), (0, 'レアリティ0 : 009番', 0),
(0, 'レアリティ0 : 010番', 0),
(1, 'レアリティ1 : 001番', 0), (1, 'レアリティ1 : 002番', 0), (1, 'レアリティ1 : 003番', 0),
(1, 'レアリティ1 : 004番', 0), (1, 'レアリティ1 : 005番', 0), (1, 'レアリティ1 : 006番', 0),
(1, 'レアリティ1 : 007番', 0), (1, 'レアリティ1 : 008番', 0), (1, 'レアリティ1 : 009番', 0),
(1, 'レアリティ1 : 010番', 0),
(2, 'レアリティ2 : 001番', 0), (2, 'レアリティ2 : 002番', 0), (2, 'レアリティ2 : 003番', 0),
(2, 'レアリティ2 : 004番', 0), (2, 'レアリティ2 : 005番', 0), (2, 'レアリティ2 : 006番', 0),
(2, 'レアリティ2 : 007番', 0), (2, 'レアリティ2 : 008番', 0), (2, 'レアリティ2 : 009番', 0),
(2, 'レアリティ2 : 010番', 0),
(3, 'レアリティ3 : 001番', 0), (3, 'レアリティ3 : 002番', 0), (3, 'レアリティ3 : 003番', 0),
(3, 'レアリティ3 : 004番', 0), (3, 'レアリティ3 : 005番', 0), (3, 'レアリティ3 : 006番', 0),
(3, 'レアリティ3 : 007番', 0), (3, 'レアリティ3 : 008番', 0), (3, 'レアリティ3 : 009番', 0),
(3, 'レアリティ3 : 010番', 0),
(4, 'レアリティ4 : 001番', 0), (4, 'レアリティ4 : 002番', 0), (4, 'レアリティ4 : 003番', 0),
(4, 'レアリティ4 : 004番', 0), (4, 'レアリティ4 : 005番', 0), (4, 'レアリティ4 : 006番', 0),
(4, 'レアリティ4 : 007番', 0), (4, 'レアリティ4 : 008番', 0), (4, 'レアリティ4 : 009番', 0),
(4, 'レアリティ4 : 010番', 0),
(5, 'レアリティ5 : 001番', 0), (5, 'レアリティ5 : 002番', 0), (5, 'レアリティ5 : 003番', 0),
(5, 'レアリティ5 : 004番', 0), (5, 'レアリティ5 : 005番', 0), (5, 'レアリティ5 : 006番', 0),
(5, 'レアリティ5 : 007番', 0), (5, 'レアリティ5 : 008番', 0), (5, 'レアリティ5 : 009番', 0),
(5, 'レアリティ5 : 010番', 0);
