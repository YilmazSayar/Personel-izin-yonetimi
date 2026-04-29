-- Permissions tablosuna Type sütununu ekler (PostgreSQL)
-- Bu dosyayı pgAdmin veya psql ile çalıştırın, ya da backend ilk açılışta otomatik çalışacak.

ALTER TABLE "Permissions" ADD COLUMN IF NOT EXISTS "Type" text;
