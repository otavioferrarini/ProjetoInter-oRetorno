CREATE DATABASE RastreamentoCoronavirus;
\c rastreamentocoronavirus
CREATE TABLE usuario(
	CPF char(11) PRIMARY KEY NOT NULL,
	Nome varchar(50) NOT NULL,
	Rua varchar(50),
	Numero varchar(6),
	Bairro varchar(30),
	CEP char(8) NOT NULL,
	Complemento varchar(15),
	Cidade varchar(30),
	Estado char(2)
);
CREATE TABLE estabelecimento(
	CNPJ char(14) PRIMARY KEY NOT NULL,
	RazaoSocial varchar(50) NOT NULL,
	Rua varchar(50),
	Numero varchar(6),
	Bairro varchar(30),
	CEP char(8) NOT NULL,
	Complemento varchar(15),
	Cidade varchar(30),
	Estado char(2)
);
CREATE TABLE checkin(
	CodCheckin serial PRIMARY KEY NOT NULL,
	CPF char(11) NOT NULL,
	CNPJ char(14) NOT NULL,
	Data date NOT NULL,
	FOREIGN KEY (CPF) REFERENCES usuario(CPF),
	FOREIGN KEY (CNPJ) REFERENCES estabelecimento(CNPJ)
);
CREATE TABLE reportado(
	CPF char(11) NOT NULL,
	Data date NOT NULL,
	FOREIGN KEY (CPF) REFERENCES usuario(CPF),
	CONSTRAINT cod_rep PRIMARY KEY (CPF, CNPJ)
);
CREATE TABLE tel_usuario(
	CPF char(11) NOT NULL,
	IDtelefone int NOT NULL,
	Telefone char(11),
	CONSTRAINT tel_us PRIMARY KEY (CPF,IDtelefone),
	FOREIGN KEY (CPF) REFERENCES usuario(CPF) 
);
CREATE TABLE tel_estabel(
	CNPJ char(14) NOT NULL,
	IDtelefone int NOT NULL,
	Telefone char(11),
	CONSTRAINT tel_es PRIMARY KEY (CNPJ,IDtelefone),
	FOREIGN KEY (CNPJ) REFERENCES estabelecimento(CNPJ) 
);

select c.CodCheckin
from usuario as u join checkin as c
on u.CPF = c.CPF;

WITH localcont AS (SELECT cnpj FROM checkin WHERE (teste = 'Positivo'))
SELECT c.cpf, e.RazaoSocial, l.cnpj
FROM checkin c LEFT JOIN estabelecimento e 
ON c.cnpj = e.cnpj 
JOIN localcont l
ON l.cnpj = c.cnpj
WHERE (c.Teste = 'Negativo');