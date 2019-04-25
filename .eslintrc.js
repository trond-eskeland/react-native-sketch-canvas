module.exports = {
    "extends": "airbnb",
    "rules": {
      "react/jsx-filename-extension": ["error", { "extensions": [".js", ".jsx"] }],
      "jsx-a11y/anchor-is-valid": ["warn", { "aspects": ["invalidHref"] }],
      "max-len": ["error", 120],
    },
  };