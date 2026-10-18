/**
 * Demo-site shortcut accounts. These are the seeded users from the server's `DbSeeder` (also listed in the README),
 * so signing in with them only works against a server that seeded the demo data.
 */
export interface DemoAccount {
  label: string
  hint: string
  email: string
  password: string
}

export const DEMO_ACCOUNTS: DemoAccount[] = [
  { label: 'כניסה כלקוח', hint: 'הזמנת ציוד וצפייה בהזמנות שלי', email: 'producer1@eventix.dev', password: 'Producer123!' },
  { label: 'כניסה כמנהל', hint: 'ניהול הזמנות, ציוד וקטגוריות', email: 'admin@eventix.dev', password: 'Admin123!' },
]
