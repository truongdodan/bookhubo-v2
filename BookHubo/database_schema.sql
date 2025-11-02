

-- Create Users table
CREATE TABLE IF NOT EXISTS users (
    userid SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    passwordhash VARCHAR(255) NOT NULL,
    fullname VARCHAR(255) NOT NULL,
    phonenumber VARCHAR(20),
    shippingaddress TEXT,
    role VARCHAR(50) DEFAULT 'User',
    isbanned BOOLEAN DEFAULT FALSE,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    averagerating DECIMAL(3,2) DEFAULT 0.0,
    totalreviews INTEGER DEFAULT 0
);

-- Create Books table
CREATE TABLE IF NOT EXISTS books (
    bookid SERIAL PRIMARY KEY,
    title VARCHAR(500) NOT NULL,
    author VARCHAR(255) NOT NULL,
    isbn VARCHAR(50),
    category VARCHAR(100),
    condition VARCHAR(50),
    price DECIMAL(10,2) NOT NULL,
    stockquantity INTEGER DEFAULT 0,
    description TEXT,
    imagepath VARCHAR(500),
    sellerid INTEGER NOT NULL,
    isactive BOOLEAN DEFAULT TRUE,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    averagerating DECIMAL(3,2) DEFAULT 0.0,
    totalreviews INTEGER DEFAULT 0,
    FOREIGN KEY (sellerid) REFERENCES users(userid) ON DELETE CASCADE
);

-- Create CartItems table
CREATE TABLE IF NOT EXISTS cartitems (
    cartitemid SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL,
    bookid INTEGER NOT NULL,
    quantity INTEGER DEFAULT 1,
    addedat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (userid) REFERENCES users(userid) ON DELETE CASCADE,
    FOREIGN KEY (bookid) REFERENCES books(bookid) ON DELETE CASCADE
);

-- Create Orders table
CREATE TABLE IF NOT EXISTS orders (
    orderid SERIAL PRIMARY KEY,
    buyerid INTEGER NOT NULL,
    totalprice DECIMAL(10,2) NOT NULL,
    shippingaddress TEXT NOT NULL,
    orderdate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (buyerid) REFERENCES users(userid) ON DELETE CASCADE
);

-- Create OrderItems table
CREATE TABLE IF NOT EXISTS orderitems (
    orderitemid SERIAL PRIMARY KEY,
    orderid INTEGER NOT NULL,
    bookid INTEGER NOT NULL,
    sellerid INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    priceatpurchase DECIMAL(10,2) NOT NULL,
    status VARCHAR(50) DEFAULT 'Pending',
    shippedat TIMESTAMP,
    completedat TIMESTAMP,
    FOREIGN KEY (orderid) REFERENCES orders(orderid) ON DELETE CASCADE,
    FOREIGN KEY (bookid) REFERENCES books(bookid) ON DELETE CASCADE,
    FOREIGN KEY (sellerid) REFERENCES users(userid) ON DELETE CASCADE
);

-- Create Reviews table
CREATE TABLE IF NOT EXISTS reviews (
    reviewid SERIAL PRIMARY KEY,
    orderitemid INTEGER NOT NULL,
    bookid INTEGER NOT NULL,
    buyerid INTEGER NOT NULL,
    sellerid INTEGER NOT NULL,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (orderitemid) REFERENCES orderitems(orderitemid) ON DELETE CASCADE,
    FOREIGN KEY (bookid) REFERENCES books(bookid) ON DELETE CASCADE,
    FOREIGN KEY (buyerid) REFERENCES users(userid) ON DELETE CASCADE,
    FOREIGN KEY (sellerid) REFERENCES users(userid) ON DELETE CASCADE,
    UNIQUE (orderitemid)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_books_sellerid ON books(sellerid);
CREATE INDEX IF NOT EXISTS idx_books_category ON books(category);
CREATE INDEX IF NOT EXISTS idx_books_isactive ON books(isactive);
CREATE INDEX IF NOT EXISTS idx_cartitems_userid ON cartitems(userid);
CREATE INDEX IF NOT EXISTS idx_orders_buyerid ON orders(buyerid);
CREATE INDEX IF NOT EXISTS idx_orderitems_orderid ON orderitems(orderid);
CREATE INDEX IF NOT EXISTS idx_orderitems_sellerid ON orderitems(sellerid);
CREATE INDEX IF NOT EXISTS idx_orderitems_status ON orderitems(status);
CREATE INDEX IF NOT EXISTS idx_reviews_bookid ON reviews(bookid);
CREATE INDEX IF NOT EXISTS idx_reviews_sellerid ON reviews(sellerid);

-- Insert sample admin user (password: admin123)
-- Note: This is a sample password hash. In production, you should generate your own secure password.
INSERT INTO users (email, passwordhash, fullname, role)
VALUES ('admin@bookhubo.com', '$2a$11$3bKjqYQXH5K8mG0YZx6.WeXjF5L.1nZ7qP9x7yJQk9jW6sH.8kQHa', 'Admin User', 'Admin')
ON CONFLICT (email) DO NOTHING;

-- Insert sample regular user (password: user123)
INSERT INTO users (email, passwordhash, fullname, phonenumber, shippingaddress)
VALUES ('user@example.com', '$2a$11$Dw9Z8XJ0qY7rH5K9mN0vLeXjF5L.1nZ7qP9x7yJQk9jW6sH.9lPIb', 'Sample User', '0123456789', '123 Nguyen Trai, Ha Noi')
ON CONFLICT (email) DO NOTHING;

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'Database schema created successfully!';
    RAISE NOTICE 'Default admin account: admin@bookhubo.com / admin123';
    RAISE NOTICE 'Default user account: user@example.com / user123';
END $$;
