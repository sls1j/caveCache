Object.deepClone = function (source) {
  let destination = source;
  if (source && typeof source === 'object') {
    destination = Object.prototype.toString.call(source) === "[object Array]" ? [] : {};
    for (var i in source) {
      destination[i] = Object.deepClone(source[i]);
    }
  }  

  return destination;
};

Object.deepExtend = function (source, destination) {
  if (source && typeof source === 'object') {    
    for (var i in source) {
      destination[i] = Object.deepClone(source[i]);
    }
  }
  else
    destination = source;  
};

Array.prototype.removeAll = function (callback) {
  for (let i = this.length - 1; i >= 0; i--) {
    let item = this[i];
    if (callback(item)) {
      this.splice(i, 1);
    }
  }
}