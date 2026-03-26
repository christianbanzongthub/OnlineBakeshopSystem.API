March 25, 2026

We change the stored procedure from SP_ONLINEBAKESHOPDB_CREATEPRODUCT and SP_ONLINEBAKESHOPDB_GETPRODUCTS into one and we call it SP_ONLINEBAKESHOPDB_PRODUCT.
One thing we do this day we add new api for products such as GetProductById, UpdateProduct, DeleteProduct and GetAllProducts. We make some changes in the database 
specifically on the tbl_products and alter the imageUrl.
ALTER TABLE tbl_products
ALTER COLUMN imageUrl NVARCHAR(MAX);
THANK YOU MS.
