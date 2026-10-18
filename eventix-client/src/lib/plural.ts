// Hebrew singular/plural for the words the UI counts. Hebrew says "יום אחד", not "1 ימים".

export const daysLabel = (n: number) => (n === 1 ? 'יום אחד' : `${n} ימים`)
export const itemsLabel = (n: number) => (n === 1 ? 'פריט אחד' : `${n} פריטים`)
export const unitsLabel = (n: number) => (n === 1 ? 'יחידה אחת' : `${n} יחידות`)
