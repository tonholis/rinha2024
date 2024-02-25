CREATE UNLOGGED TABLE IF NOT EXISTS customers (
    "id"                SERIAL,
    "limit"             INT NOT NULL,
    "balance"           INT DEFAULT 0,

    PRIMARY KEY (id)
);

CREATE UNLOGGED TABLE IF NOT EXISTS transactions (
    "id"           SERIAL,
    "customer_id"  INT NOT NULL,
    "amount"       INT NOT NULL,
    "type"         VARCHAR(1) NOT NULL,
    "description"  VARCHAR(10) NOT NULL,
    "created_at"   TIMESTAMP,
    
    CONSTRAINT "customer_fk" FOREIGN KEY ("customer_id") REFERENCES customers("id")
);

DO $$
BEGIN
  INSERT INTO customers ("limit")
  VALUES
    (1000 * 100),
    (800 * 100),
    (10000 * 100),
    (100000 * 100),
    (5000 * 100);
END; $$
