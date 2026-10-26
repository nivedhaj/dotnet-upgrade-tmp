SELECT 'CREATE DATABASE ct_modular_test OWNER ct'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ct_modular_test')\gexec

\c ct_modular
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c ct_modular_test
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
