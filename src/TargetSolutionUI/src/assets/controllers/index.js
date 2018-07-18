const express = require('express');
const app = express();

const launchpadRoute = require('./_ref/launchpad-api');
const entityRoute = require('./_ref/entity-api');
const moduleRoute = require('./adas/module-api');

app.get('/', (req, res) => {
        res.json({ item: 'Dover Launchpad iIoT RESTful API - Version 1.0.0' });
    })
    .use('/help', launchpadRoute)
    .use('/entity', entityRoute)
    .use('/module', moduleRoute);

module.exports = app;