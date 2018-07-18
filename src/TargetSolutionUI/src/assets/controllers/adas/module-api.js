const express = require('express');
const app = express();
const swaggerUi = require('swagger-ui-express');
const swaggerDocument = require('./module-doc.json');

app.use('/docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument));

app.get('/', (req, res) => {
    res.json({ item: 'ADAS Module API - Version 1.0.0' });
});

var fs = require('fs');
var path = require('path');

var modulesListPath = path.join('./data/files/adas', 'modules_list.json');

app.get('/list', (req, res) => {
    var readable = fs.createReadStream(modulesListPath);
    readable.pipe(res);
});

var where = require("lodash.where");
var modulesKeysPath = path.join('./data/files/adas', 'modules_keys.json');
var keys = JSON.parse(fs.readFileSync(modulesKeysPath, 'utf8'));

app.get('/key/:moduleId', (req, res) => {
    var moduleId = req.params.moduleId;
    var keyObj = where(keys, { "module_id": moduleId });

    console.log('On module API /key moduleId[%s]  result->%s', moduleId, JSON.stringify(keyObj));

    if (keyObj.length == 0)
        console.log('On module API /key value of keys[%s]', JSON.stringify(keys));

    res.json(keyObj);
});

app.get('/keys', (req, res) => {
    var moduleIds = req.query.ids;
    var arr = moduleIds.split(".");
    var result = '[';
    var firstItem = true;

    for (var index = 0; index < arr.length; index++) {
        var keyObj = where(keys, { "module_id": arr[index] });
        if (keyObj.length > 0) {
            if (firstItem)
                firstItem = false;
            else
                result += ',';

            result += JSON.stringify(keyObj[0]);
        }
    }

    result += ']';

    console.log('On module API /keys ids[%s]  result->%s', moduleIds, result);

    res.json(JSON.parse(result));
});

module.exports = app;