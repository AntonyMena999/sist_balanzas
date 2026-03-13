"""
Simulador de BalanzaService para desarrollo local.
Imita exactamente los endpoints reales: /health, /status, /balanza
Uso: python balanza_simulada.py
Luego agrega 127.0.0.1 o tu IP (172.16.36.155) en el monitor.
"""

from http.server import HTTPServer, BaseHTTPRequestHandler
import json
import random
import datetime

PESO_FIJO = None  # None = peso aleatorio, o pon un numero ej: 12.5

class BalanzaHandler(BaseHTTPRequestHandler):

    def do_GET(self):
        if self.path == "/health":
            self._responder(200, "OK", content_type="text/plain")

        elif self.path == "/status":
            peso = PESO_FIJO if PESO_FIJO else round(random.uniform(1.0, 50.0), 2)
            data = {
                "peso": str(peso),
                "fecha": datetime.datetime.utcnow().isoformat() + "Z"
            }
            self._responder(200, json.dumps(data))

        elif self.path == "/balanza":
            peso = PESO_FIJO if PESO_FIJO else round(random.uniform(1.0, 50.0), 2)
            data = {"peso": str(peso)}
            self._responder(200, json.dumps(data))

        else:
            self._responder(404, "Not found", content_type="text/plain")

    def _responder(self, codigo, body, content_type="application/json"):
        self.send_response(codigo)
        self.send_header("Content-Type", content_type)
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(body.encode())

    def log_message(self, format, *args):
        print(f"  [{datetime.datetime.now().strftime('%H:%M:%S')}] {args[0]} {args[1]}")

if __name__ == "__main__":
    PORT = 80
    print(f"Balanza simulada corriendo en http://localhost:{PORT}")
    print(f"  /health  → OK")
    print(f"  /status  → peso + fecha")
    print(f"  /balanza → peso")
    print(f"\nAgrégala en el monitor como IP: 127.0.0.1 o 172.16.36.155")
    print(f"Ctrl+C para detener\n")
    try:
        HTTPServer(("0.0.0.0", PORT), BalanzaHandler).serve_forever()
    except PermissionError:
        print("ERROR: Puerto 80 requiere permisos. Ejecuta como administrador.")
        print("O cambia PORT = 80 por PORT = 8080 y agrega la balanza como 127.0.0.1:8080")