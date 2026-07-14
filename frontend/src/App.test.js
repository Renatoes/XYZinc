import { render, screen } from '@testing-library/react';
import App from './App';

test('renders billing form', () => {
  render(<App />);
  expect(screen.getByText(/XYZ Inc. Billing/i)).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /submit order/i })).toBeInTheDocument();
});
