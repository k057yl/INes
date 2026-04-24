export const FORM_VALIDATION = {
  PRICE: {
    MIN: 0.01
  },
  DATE: {
    get TODAY() { return new Date().toISOString().substring(0, 10); }
  }
};