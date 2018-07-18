const express = require('express');
const app = express();


app.get('/', (req, res) => {
    console.log('On launchpad API - GET / ');
    res.json({ item: 'Dover Launchpad API HELP' });
});


module.exports = app;