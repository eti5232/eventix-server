import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { categoriesApi } from '../api/endpoints'
import { CategoryIcon } from '../components/CategoryIcon'
import { categoryLook, categoryTone } from '../lib/categories'
import styles from './HomePage.module.css'

const FACTS = ['זמינות מתעדכנת לפי יום', 'השכרה של עד 7 ימים ברצף', 'ביטול עד תחילת האירוע']

const STEPS = [
  { title: 'בוחרים תאריכים', text: 'מסמנים את ימי האירוע, וכל הקטלוג מציג זמינות לטווח הזה.', tone: 'Speakers' },
  { title: 'מוסיפים ציוד להזמנה', text: 'בוחרים כמויות לפי מה שפנוי בכל יום של האירוע.', tone: 'Microphones' },
  { title: 'מאשרים הזמנה', text: 'ההזמנה נשמרת כולה או לא נשמרת בכלל, בלי הזמנות חלקיות.', tone: 'Lighting' },
]

export function HomePage() {
  const categories = useQuery({ queryKey: ['categories'], queryFn: categoriesApi.list, staleTime: 5 * 60_000 })

  return (
    <div className={styles.home}>
      <section className={styles.hero} aria-labelledby="headline">
        <div className={styles.heroInner}>
          <div className={styles.copy}>
            <p className={styles.eyebrow}>השכרת ציוד הפקה</p>
            <h1 id="headline" className={styles.headline}>
              ציוד הפקה
              <br />
              לכל <mark>אירוע</mark>
            </h1>
            <p className={styles.lead}>
              מערכות קול, תאורה, הקרנה ובמות להשכרה. בוחרים תאריכי אירוע, רואים מה פנוי, ומזמינים באתר.
            </p>
            <div className={styles.cta}>
              <Link to="/equipment" className={`btn btn-primary ${styles.big}`}>
                לקטלוג הציוד
              </Link>
              <a href="#how" className={styles.textLink}>
                איך זה עובד ←
              </a>
            </div>
            <ul className={styles.facts}>
              {FACTS.map((fact) => (
                <li key={fact}>{fact}</li>
              ))}
            </ul>
          </div>

          {/* Decorative collage: colored tiles with the category icons. */}
          <div className={styles.collage} aria-hidden="true">
            <span className={styles.blob} />
            <div className={`${styles.card} ${styles.cardA}`} style={categoryTone('Speakers')}>
              <CategoryIcon name="Speakers" className={styles.collageIcon} />
            </div>
            <div className={`${styles.card} ${styles.cardB}`} style={categoryTone('Lighting')}>
              <CategoryIcon name="Lighting" className={styles.collageIcon} />
            </div>
            <div className={`${styles.card} ${styles.cardC}`} style={categoryTone('Microphones')}>
              <CategoryIcon name="Microphones" className={styles.collageIcon} />
            </div>
            <span className={styles.dot} />
          </div>
        </div>
      </section>

      <section className={styles.categories} aria-labelledby="cats">
        <div className={styles.sectionHead}>
          <h2 id="cats">קטגוריות ציוד</h2>
          <Link to="/equipment" className={styles.allLink}>
            לכל הקטלוג ←
          </Link>
        </div>

        <div className={styles.tiles}>
          {categories.isLoading &&
            Array.from({ length: 5 }, (_, i) => <div key={i} className={`${styles.tile} skeleton`} />)}
          {categories.isError && (
            <p className={styles.note}>לא הצלחנו לטעון קטגוריות כרגע. אפשר להמשיך ישירות לקטלוג.</p>
          )}
          {categories.data?.map((category) => {
            const look = categoryLook(category.name)
            return (
              <Link
                key={category.id}
                to={`/equipment?category=${category.id}`}
                className={styles.tile}
                style={categoryTone(category.name)}
              >
                <CategoryIcon name={category.name} className={styles.icon} />
                <span className={styles.label}>
                  <strong>{look.label}</strong>
                  <span className={styles.go}>לצפייה בציוד ←</span>
                </span>
              </Link>
            )
          })}
        </div>
      </section>

      <section id="how" className={styles.how} aria-labelledby="how-title">
        <div className={styles.howInner}>
          <h2 id="how-title">איך משכירים</h2>
          <ol className={styles.steps}>
            {STEPS.map((step, index) => (
              <li key={step.title} style={categoryTone(step.tone)}>
                <span className={styles.stepNo} aria-hidden="true">
                  {index + 1}
                </span>
                <h3>{step.title}</h3>
                <p>{step.text}</p>
              </li>
            ))}
          </ol>
        </div>
      </section>

      <section className={styles.closing} aria-labelledby="closing">
        <div className={styles.closingInner}>
          <h2 id="closing">מתחילים להרכיב את ההזמנה</h2>
          <Link to="/equipment" className={styles.closingButton}>
            לקטלוג הציוד
          </Link>
        </div>
      </section>
    </div>
  )
}
