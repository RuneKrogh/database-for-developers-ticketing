CREATE TABLE IF NOT EXISTS users (
    id    SERIAL PRIMARY KEY,
    name  TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS events (
    id               SERIAL PRIMARY KEY,
    name             TEXT NOT NULL,
    total_seats      INT  NOT NULL,
    available_seats  INT  NOT NULL CHECK (available_seats >= 0),
    version          INT  NOT NULL DEFAULT 0,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS bookings (
    id        SERIAL PRIMARY KEY,
    event_id  INT NOT NULL REFERENCES events(id),
    user_id   INT NOT NULL REFERENCES users(id),
    booked_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

INSERT INTO users (name, email) VALUES
    ('Alice',   'alice@example.com'),
    ('Bob',     'bob@example.com'),
    ('Charlie', 'charlie@example.com'),
    ('Diana',   'diana@example.com'),
    ('Eve',     'eve@example.com')
ON CONFLICT (email) DO NOTHING;

INSERT INTO events (name, total_seats, available_seats) VALUES
    ('Rock Concert',      100, 100),
    ('Championship Final',  2,   2),
    ('Theatre Night',      50,  50)
ON CONFLICT DO NOTHING;
