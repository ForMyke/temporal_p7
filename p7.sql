CREATE TABLE stock (
                       id_articulo INT PRIMARY KEY AUTO_INCREMENT,
                       nombre      VARCHAR(255) NOT NULL,
                       descripcion VARCHAR(255),
                       precio      DECIMAL(10,2) NOT NULL,
                       cantidad    INT DEFAULT 0
);

CREATE TABLE fotos_articulos (
                                 id_foto     INT PRIMARY KEY AUTO_INCREMENT,
                                 fotografia  LONGBLOB,
                                 id_articulo INT,
                                 CONSTRAINT fk_fotos_stock
                                     FOREIGN KEY (id_articulo) REFERENCES stock(id_articulo)
                                         ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE carrito_compra (
                                id_usuario  INT NOT NULL,
                                id_articulo INT NOT NULL,
                                cantidad    INT DEFAULT 1,
                                CONSTRAINT fk_carrito_stock
                                    FOREIGN KEY (id_articulo) REFERENCES stock(id_articulo)
                                        ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE UNIQUE INDEX index_carrito_compra
    ON carrito_compra (id_usuario, id_articulo);

DESCRIBE stock;
SHOW INDEX FROM carrito_compra;