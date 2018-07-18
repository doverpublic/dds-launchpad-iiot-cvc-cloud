var createError = require('http-errors');
var express = require('express');
var path = require('path');
var favicon = require('serve-favicon');
var logger = require('morgan');
var apiRouter = require('./dist/TargetSolutionUI/assets/controllers');

// Enable telemetry collection with Application Insights
var ai = require('applicationinsights');
ai.setup(process.env.APPLICATIONINSIGHTSKEY || 'a42a6680-a0af-43f7-9a24-e2783e88e607').start();

var app = new express();

app.use(express.urlencoded({ extended: false }));
app.use(logger('dev'));
app.use(express.json());
app.use(express.static(path.join(__dirname, 'dist/TargetSolutionUI')));
app.use('/', express.static(path.join(__dirname, 'dist/TargetSolutionUI')));
app.use('/api', apiRouter);

// catch 404 and forward to error handler
app.use(function(req, res, next) {
    next(createError(404));
});

// error handler
app.use(function(err, req, res, next) {
    // set locals, only providing error in development
    res.locals.message = err.message;
    res.locals.error = req.app.get('env') === 'development' ? err : {};

    // render the error page
    res.status(err.status || 500);
    res.sendStatus(err.status);
});

module.exports = app;