-- create database DBLibreria;

use DBLibreria;

create table usuarios(
    id int NOT NULL identity primary key,
	imagen varchar(250) NULL,
	nombre varchar(100) NOT NULL,
	apellido varchar(100) NOT NULL,
	email varchar(40) NOT NULL,
	clave varchar(100) NOT NULL,
	rol varchar(20) NOT NULL
)

create table proveedores(
    id int NOT NULL identity primary key,
	nombre varchar(100) NOT NULL,
	email varchar(35) NOT NULL,
	telefono int NOT NULL,
	direccion varchar(250) NOT NULL
)

create table productos(
	id int NOT NULL identity primary key,
	imagen varchar(250) NULL,
	nombre varchar(100) NOT NULL,
	precio float NOT NULL,
	descripcion varchar(250) NOT NULL,
	stock int NOT NULL
)

create table carrito(
	id int NOT NULL identity primary key,
	imagen varchar(250) NULL,
	nombre varchar(100) NOT NULL,
	precio float NOT NULL,
	descripcion varchar(250) NOT NULL,
	cantidad int NOT NULL,
	id_usuario int NOT NULL,
	id_producto int NOT NULL
)

insert into productos(imagen, nombre, precio, descripcion, stock) values 
('birome.png','Birome Bic', 210.3, 'Birome trazo fino color azul', 8),
('cinta.png','Cinta embalar', 350.7, 'Cinta ancha apta para embalaje color transparente', 4),
('marcador.png','Marcador', 138.5, 'Marcador trazo grueso permanente color negro', 12)

insert into proveedores(nombre, email, telefono, direccion) values 
('Papelera Plata', 'papelera_plata@gmail.com', 45632014, 'Av. Mitre 1234'),
('Lopez Hermanos', 'lhermanos@outlook.com', 40879954, 'Neuquen 257, 5° A'),
('Distribuidora Gonzalez', 'distribuidora_gonzalez@gmail.com', 1178985043, 'Av. Forest 2541')

select * from Carrito;

select * from Productos;

select * from Usuarios;

select * from Proveedores;

