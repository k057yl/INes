export const FORM_VALIDATION = {
  PRICE: {
    MIN: 0.01
  },
  DATE: {
    // Хелпер для текущей даты в формате YYYY-MM-DD
    get TODAY() { return new Date().toISOString().substring(0, 10); }
  }
};