CREATE TABLE IF NOT EXISTS customers (
    "id"                SERIAL,
    "limit"             INT NOT NULL,
    "balance"           INT DEFAULT 0,

    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS transactions (
    "id"           SERIAL,
    "customer_id"  INT NOT NULL,
    "amount"       INT NOT NULL,
    "type"         VARCHAR(1) NOT NULL,
    "description"  VARCHAR(10) NOT NULL,
    "created_at"   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT "customer_fk" FOREIGN KEY ("customer_id") REFERENCES customers("id")
);

DO $$
BEGIN
  INSERT INTO customers ("limit")
  VALUES
    (100000),
    (80000),
    (1000000),
    (10000000),
    (500000);
END; $$
