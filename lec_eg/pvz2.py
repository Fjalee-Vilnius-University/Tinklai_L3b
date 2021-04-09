#!/usr/bin/env python
"""
 Paprasto serverio pavyzdys.
 Išbandyta Linux (Ubuntu) sistemoje. Windows sistemoje gali neveikti.
 Krauna index, bet kitur neeina
"""

import socket


SERVERIO_IP = '127.0.0.1'
SERVERIO_PORTAS = 10000

serverioSoketas = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serverioSoketas.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
serverioSoketas.bind((SERVERIO_IP, SERVERIO_PORTAS))
serverioSoketas.listen(1)  # vieno kliento

print('Bandome klausyti %s ...' % SERVERIO_PORTAS)

while True:    
    
    kliento_jungtis, kliento_adresas = serverioSoketas.accept()

    uzklausa = kliento_jungtis.recv(1024).decode()
    print(uzklausa)

    # Bandome  pateikti index.html
    failas = open('index.html', 'rb')
    turinys = failas.read()
    failas.close()

    atsakas = b'HTTP/1.0 200 OK\n\n' + turinys
    kliento_jungtis.sendall(atsakas)
    kliento_jungtis.close()


serverioSoketas.close()

