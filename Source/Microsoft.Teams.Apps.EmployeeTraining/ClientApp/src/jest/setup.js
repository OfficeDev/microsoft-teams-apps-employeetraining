// Import adapter for enzyme
var enzyme = require('enzyme');
var Adapter = require('enzyme-adapter-react-16');
enzyme.configure({ adapter: new Adapter() })

// Log all jsDomErrors when using jsdom testEnvironment
window._virtualConsole && window._virtualConsole.on('jsdomError', function (error) {
    console.error('jsDomError', error.stack, error.detail);
});
