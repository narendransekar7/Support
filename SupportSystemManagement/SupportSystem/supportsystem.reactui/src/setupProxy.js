const { createProxyMiddleware } = require("http-proxy-middleware");

// Dev-server only: forwards /api to the gateway so the UI never needs an absolute gateway URL.
module.exports = function (app) {
  app.use(
    "/api",
    createProxyMiddleware({
      target: process.env.DEV_GATEWAY_URL || "https://localhost:44345",
      changeOrigin: true,
      secure: false, // accept the Visual Studio dev certificate
    })
  );
};
