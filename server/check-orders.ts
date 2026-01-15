
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  const orders = await prisma.order.findMany({
    select: { createdTime: true }
  });

  console.log(`Total orders: ${orders.length}`);

  const counts: Record<string, number> = {};
  orders.forEach(o => {
    const key = o.createdTime.toISOString().split('T')[0];
    counts[key] = (counts[key] || 0) + 1;
  });

  console.log('Orders per day:', counts);
}

main()
  .catch(e => console.error(e))
  .finally(async () => await prisma.$disconnect());
