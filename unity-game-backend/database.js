const { hashSync } = require('bcrypt');

const sqlite3 = require ('sqlite3').verbose();  //verbose to enable additional debugging for sql queries

const db = new sqlite3.Database("./game.db", (err) =>{
    if(err) console.log(err.message);
    console.log("Connected to SQLite database");
});

const createTables = () => {
    db.run (`CREATE TABLE IF NOT EXISTS users(
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        username TEXT UNIQUE,
        password TEXT
        );`);

    db.run (`CREATE TABLE IF NOT EXISTS progress (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        user_id INTEGER,
        health INTEGER,
        position TEXT,
        FOREIGN KEY(user_id) REFERENCES users(id))`);
};

const registerUser = (username, hashedPassword, callback) => {
    db.run (
        `INSERT INTO users (username, password) VALUES (?,?)`,
        [username, hashedPassword],
        function (err){
            callback(err, this);
        }
    )
}

const getUserByUsername = (username, callback) => {
    db.get(`SELECT * FROM users WHERE username = ?`, [username], (err,row) =>{   // ?`, [username] to prevent sql injection
        callback(err,row);
    })
}

const saveProgress = (userId, health, position, callback) => {
    
    db.run(`INSERT INTO progress (user_id, health, position) VALUES (?,?,?)`,[userId, health, position],
        function(err){
            if(err) console.error("DB Save error: ", err.message);
            callback(err,this);
        }
    )
}

const loadProgress = (userId, callback) =>{
    db.get (
        `SELECT * FROM progress WHERE user_id = ? ORDER BY id DESC LIMIT 1`,
        [userId],
        (err, rows) =>{
            callback (err, rows);
        }
    )
}

module.exports = {
    db,
    createTables,
    registerUser,
    getUserByUsername,
    saveProgress,
    loadProgress
};