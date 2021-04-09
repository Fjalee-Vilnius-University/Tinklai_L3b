#!/usr/bin/env python
"""
  Paprasto serverio pavyzdys
  Išbandyta Linux (Ubuntu) sistemoje. Windows sistemoje gali neveikti.
  Krauna statinius failus, geba atlikti CGI  veiskmą 
  per GET ir POST
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
    print("Klientas (naršyklė): ", uzklausa)
    print("-----------------------------------------------")
    # Reikia išanalizuoti antraštę:
    antraste = uzklausa.split('\n')
    # ..  failo vardas bus antras „žodis“:
    pirmaEilute = antraste[0].split()
    arPost =  pirmaEilute[0].upper() == 'POST'
    failoVardas = pirmaEilute[1]
    print(failoVardas)

    if failoVardas == '/':
        failoVardas = '/index.html'

    # Gal cgi scenarijus?
    if failoVardas[:len('/cgi-bin/')] == '/cgi-bin/':
         skriptasIrParametrai = failoVardas[len('/cgi-bin/'):].split('?')
         skriptoVardas = skriptasIrParametrai[0]
         if arPost:
            skriptoParametrai = antraste[-1]
         else:
            skriptoParametrai = skriptasIrParametrai[1]
         skriptoParametrai = skriptoParametrai.replace('&',' ')
         print('./cgi-bin/'+skriptoVardas)
         vykdymas = subprocess.run(['./cgi-bin/'+skriptoVardas, skriptoParametrai, "/dev/null"], capture_output=True)
         atsakas = 'HTTP/1.0 200 OK\n\n' + vykdymas.stdout.decode("utf-8")
         kliento_jungtis.sendall(atsakas.encode())
         kliento_jungtis.close()   
         continue
    # Krauname turinį
    try:
       failas = open('.'+failoVardas, 'rb')
       turinys = failas.read()
       failas.close()
    except IOError:
       turinys = b""



    if turinys != "":
        atsakas = b'HTTP/1.0 200 OK\n\n' + turinys
    else:
        atsakas = b'HTTP/1.0 400 NOT FOUND\n\n' + b'Neradome ...'
    #kliento_jungtis.sendall(atsakas.encode())
    kliento_jungtis.sendall(atsakas)
    kliento_jungtis.close()


serverioSoketas.close()

