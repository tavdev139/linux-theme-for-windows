// ============================================
// Simple Local Server for Samsung Linux Theme
// ============================================

const http = require('http');
const fs = require('fs');
const path = require('path');
const os = require('os');

const PORT = 8080;
const MIME_TYPES = {
    '.html': 'text/html',
    '.css': 'text/css',
    '.js': 'application/javascript',
    '.json': 'application/json',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.svg': 'image/svg+xml',
    '.ico': 'image/x-icon'
};

function getLocalIp() {
    const interfaces = os.networkInterfaces();
    for (const name of Object.keys(interfaces)) {
        for (const net of interfaces[name]) {
            if (net.family === 'IPv4' && !net.internal) {
                return net.address;
            }
        }
    }
    return '127.0.0.1';
}

const server = http.createServer((req, res) => {
    let filePath = path.join(__dirname, req.url === '/' ? 'index.html' : req.url.split('?')[0]);
    const ext = path.extname(filePath).toLowerCase();
    const contentType = MIME_TYPES[ext] || 'application/octet-stream';

    fs.readFile(filePath, (err, content) => {
        if (err) {
            if (err.code === 'ENOENT') {
                res.writeHead(404, { 'Content-Type': 'text/plain' });
                res.end('404 Not Found');
            } else {
                res.writeHead(500, { 'Content-Type': 'text/plain' });
                res.end(`Server Error: ${err.code}`);
            }
        } else {
            res.writeHead(200, { 'Content-Type': contentType });
            res.end(content, 'utf-8');
        }
    });
});

server.listen(PORT, () => {
    const localIp = getLocalIp();
    console.log('========================================================');
    console.log('🐧 SAMSUNG LINUX THEME SERVER BERJALAN!');
    console.log('========================================================');
    console.log(`💻 DI KOMPUTER : Buka http://localhost:${PORT}`);
    console.log(`📱 DI HP SAMSUNG: Buka http://${localIp}:${PORT}`);
    console.log('   (Pastikan HP dan komputer terhubung ke Wi-Fi yang sama)');
    console.log('========================================================');
    console.log('Tekan Ctrl + C di jendela ini untuk menghentikan server.');
});
