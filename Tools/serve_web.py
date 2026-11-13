#!/usr/bin/env python3
"""Serve a completed Unity Web build locally. Does not build the project."""
from pathlib import Path
from http.server import ThreadingHTTPServer,SimpleHTTPRequestHandler
import argparse,os
p=argparse.ArgumentParser();p.add_argument('--port',type=int,default=8000);args=p.parse_args()
root=Path(__file__).resolve().parents[1]/'Builds/Web'
if not (root/'index.html').exists():raise SystemExit('Build ausente. Execute Olhos de Cacador > 3. Compilar Web no Unity.')
os.chdir(root)
class Handler(SimpleHTTPRequestHandler):
 extensions_map={**SimpleHTTPRequestHandler.extensions_map,'.wasm':'application/wasm','.data':'application/octet-stream','.unityweb':'application/octet-stream'}
print(f'Abra http://localhost:{args.port}/ ; Ctrl+C encerra.')
ThreadingHTTPServer(('127.0.0.1',args.port),Handler).serve_forever()
