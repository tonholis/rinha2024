CREATE TABLE IF NOT EXISTS customers (
    "id"                SERIAL,
    "name"              VARCHAR(50) NOT NULL,
    "limit"             INT NOT NULL,
    "balance"           INT DEFAULT 0,

    PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS transactions (
    "id"           SERIAL,
    "amount"       INT NOT NULL,
    "type"         VARCHAR(1) NOT NULL,
    "description"  VARCHAR(10) NOT NULL,
    "created_at"   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    "customer_id"    INT NOT NULL,

    CONSTRAINT "customer_fk" FOREIGN KEY ("customer_id") REFERENCES customers("id")
);


DO $$
BEGIN
  INSERT INTO customers ("name", "limit")
  VALUES
    ('Cesharperia', 1000 * 100),
    ('Tacacaria', 800 * 100),
    ('Pamonharia', 10000 * 100),
    ('Pancadaria', 100000 * 100),
    ('Coxinharia', 5000 * 100);
END; $$
