#!/usr/bin/env python

"""
 Paprasto serverio pavyzdys.
  Išbandyta Linux (Ubuntu) sistemoje. Windows sistemoje gali neveikti.
 Gali krauti index ir kitus failus,,,
 CGI scenarijaus dar nevykdo
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

    # Reikia išanalizuoti antraštę:
    antraste = uzklausa.split('\n')
    # ..  failo vardas bus antras „žodis“:
    failoVardas = antraste[0].split()[1]


    if failoVardas == '/':
        failoVardas = '/index.html'

    # Krauname turinį
    try:
       failas = open('.'+failoVardas, 'rb')
       turinys = failas.read()
    except IOError:
       turinys = b""
    finally:
       failas.close()



    if turinys != b"":
        atsakas = b'HTTP/1.0 200 OK\n\n' + turinys
    else:
        atsakas = b'HTTP/1.0 400 NOT FOUND\n\n' + b'Neradome ...'
    kliento_jungtis.sendall(atsakas)
    kliento_jungtis.close()


serverioSoketas.close()

