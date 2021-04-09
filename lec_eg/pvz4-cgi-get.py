#!/usr/bin/env python
"""  
 Paprasto serverio pavyzdys.
 Išbandyta Linux (Ubuntu) sistemoje. Windows sistemoje gali neveikti.
 Krauna statinius failus, geba atlikti CGI  veiskmą 
 per GET, bet su POST dar neveikia	
"""

import socket
import subprocess


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
    print(failoVardas)

    if failoVardas == '/':
        failoVardas = '/index.html'

    # Gal cgi scenarijus?
    if failoVardas[:len('/cgi-bin/')] == '/cgi-bin/':
         skriptasIrParametrai = failoVardas[len('/cgi-bin/'):].split('?')
         skriptoVardas = skriptasIrParametrai[0]
         skriptoParametrai = skriptasIrParametrai[1]
         skriptoParametrai = skriptoParametrai.replace('&',' ')
         print('./cgi-bin/'+skriptoVardas)
         vykdymas = subprocess.run(['./cgi-bin/'+skriptoVardas, skriptoParametrai, "/dev/null"], capture_output=True)
         atsakas = 'HTTP/1.0 200 OK\n\n' + vykdymas.stdout.decode("utf-8")
         kliento_jungtis.sendall(atsakas.encode()) # str -> binary
         kliento_jungtis.close()   
         continue
    # Krauname turinį
    try:
       failas = open('.'+failoVardas, 'rb')
       turinys = failas.read()
       failas.close()
    except IOError:
       turinys = b""



    if turinys != b"":
        atsakas = b'HTTP/1.0 200 OK\n\n' + turinys
    else:
        atsakas = b'HTTP/1.0 400 NOT FOUND\n\n' + b'Neradome ...'
    #kliento_jungtis.sendall(atsakas.encode())
    kliento_jungtis.sendall(atsakas)
    kliento_jungtis.close()


serverioSoketas.close()

