import {
  require_react
} from "./chunk-VUPZMC2T.js";
import {
  __toESM
} from "./chunk-EQCVQC35.js";

// node_modules/react-bootstrap/esm/ElementChildren.js
var React = __toESM(require_react());
function map(children, func) {
  let index = 0;
  return React.Children.map(children, (child) => React.isValidElement(child) ? func(child, index++) : child);
}
function forEach(children, func) {
  let index = 0;
  React.Children.forEach(children, (child) => {
    if (React.isValidElement(child)) func(child, index++);
  });
}
function hasChildOfType(children, type) {
  return React.Children.toArray(children).some((child) => React.isValidElement(child) && child.type === type);
}

export {
  map,
  forEach,
  hasChildOfType
};
//# sourceMappingURL=chunk-ML2KYCAZ.js.map
