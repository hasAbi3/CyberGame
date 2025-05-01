const express = require ('express');
const cors = require ('cors');
const bcrypt = require ('bcrypt');
const jwt = require ('jsonwebtoken');

const {createTables, registerUser, getUserByUsername, saveProgress, loadProgress } = require("./database");

const app = express();
const PORT = 3000;
const SECRET_KEY = "abishaghimire";


//middleware setup
app.use(express.json());
app.use(cors());  //unity req allow

app.use(express.urlencoded({extended:true}));//for form parsing 

createTables();

//Register User
app.post("/register", async(req, res) =>{
    const {username, password}  = req.body;
    const hashedPassword = await bcrypt.hash(password,10);

    registerUser(username, hashedPassword, (err, result) => {
        if(err) return res.status(400).json({error: "Username already exists."});

        getUserByUsername(username, (err, user) => {
            if (!user) return res.status(500).json({ error: "User created but failed to fetch user ID." });
    
            const initialPosition = JSON.stringify({ x: 0, y: 0 });
            saveProgress(user.id, 5, initialPosition, (err) => {
                if (err) console.error("Failed to initialize progress for new user.");
            });
        });

        res.json({message: "User registered! "});
    });
});

//Login User
app.post ("/login", async (req, res)=>{
    const {username, password} = req.body;

    getUserByUsername(username, async(err, user ) =>{
        if (!user) return res.status(400).json({error: "User not found"});

        const validPassword = await bcrypt.compare(password, user.password);
        if(!validPassword) return res.status(400).json({error: "Invalid password"});

        const token = jwt.sign({id:user.id, username: user.username}, SECRET_KEY, {expiresIn: "1h"});
        res.json({token});
    });
});

//Save game progress
app.post("/save-progress", (req, res)=>{

    const authHeader = req.headers.authorization;
    const token = authHeader?.split(" ")[1];

    
    
    const {health, position} = req.body;

    if(!token || health=== undefined || !position)
    {
        return res.json({error: "Missing data: token, health, or position."})
    }
    try {
        const userData = jwt.verify(token, SECRET_KEY);
        const positionStr = typeof position === "string" ? position : JSON.stringify(position);

        saveProgress(userData.id, health, positionStr, (err, result) => {
            if (err) return res.status(500).json({ error: "Error saving progress." });
            res.json({ message: "Progress saved successfully." });
        });
    } catch (err) {
        res.status(401).json({ error: "Invalid or expired token." });
    }
});


app.get("/load-progress",(req, res)=>{
    const authHeader = req.headers.authorization;
    const token = authHeader?.split(" ")[1];

    console.log(token);
     
    if (!token) return res.status(401).json({ error: "Missing token." });


    try{
        console.log("hi");
        const userData = jwt.verify(token,SECRET_KEY);
        console.log("Decoded Token Data:", userData);
        loadProgress(userData.id, (err, progress) =>{
            if(err) return res.status(400).json({error: "Error loading progress"});

            if (!progress) return res.status(404).json({ message: "No progress found." });
            
            console.log(progress);
            
            if (!progress || progress.length === 0) {
                return res.status(404).json({ message: "No progress found" });
            }

            // Parse position JSON string back to object
            try {
                progress.position = JSON.parse(progress.position);
                console.log(progress.position);
            } catch (e) {
                progress.position = null;
            }

            return res.json(progress);
        });

    }catch(err){
        console.error("Error verifying token:", err);
        res.status(401).json({error: "Invalid token"});
    }
});

app.listen(PORT, () => console.log(`Server running on http://localhost:${PORT}`));