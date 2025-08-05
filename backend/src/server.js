const express = require('express');
const app = express();
const router = express.Router();
const dotenv = require('dotenv');
const PORT = 3000;

dotenv.config();
app.use('/', router);

app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`);
});

router.get('/', (req, res) => {
  res.send("hi");
});