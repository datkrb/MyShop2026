-- CreateTable
CREATE TABLE "shop_license" (
    "id" SERIAL NOT NULL,
    "licenseKey" TEXT,
    "trialStartDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "trialDays" INTEGER NOT NULL DEFAULT 15,
    "isActivated" BOOLEAN NOT NULL DEFAULT false,
    "activatedAt" TIMESTAMP(3),
    "activatedBy" INTEGER,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "shop_license_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "shop_license_licenseKey_key" ON "shop_license"("licenseKey");
