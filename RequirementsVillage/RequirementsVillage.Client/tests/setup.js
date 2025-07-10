// Setup jsdom for browser environment simulation
require('jsdom-global')();

// Mock localStorage for tests
global.localStorage = {
  getItem: function(key) {
    return this[key] || null;
  },
  setItem: function(key, value) {
    this[key] = value.toString();
  },
  removeItem: function(key) {
    delete this[key];
  },
  clear: function() {
    for (let key in this) {
      if (this.hasOwnProperty(key) && typeof this[key] !== 'function') {
        delete this[key];
      }
    }
  }
};

// Mock fetch if not available
if (!global.fetch) {
  global.fetch = require('node-fetch');
}

// Add any other global mocks or setup needed for tests
console.log('Test environment setup complete');