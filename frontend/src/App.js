import { useState } from 'react';
import './App.css';

const API_BASE = process.env.REACT_APP_API_URL ?? 'http://localhost:5012';

const initialForm = {
  orderNumber: '',
  userId: '',
  payableAmount: '',
  paymentGatewayId: 'stripe',
  description: '',
};

function validateForm(form) {
  const errors = {};

  if (!form.orderNumber.trim()) {
    errors.orderNumber = 'Order number is required.';
  }

  if (!form.userId.trim()) {
    errors.userId = 'User ID is required.';
  }

  const amount = Number(form.payableAmount);
  if (!form.payableAmount || Number.isNaN(amount) || amount <= 0) {
    errors.payableAmount = 'Enter a valid amount greater than zero.';
  }

  if (!form.paymentGatewayId) {
    errors.paymentGatewayId = 'Select a payment gateway.';
  }

  return errors;
}

function formatTimestamp(value) {
  if (!value) {
    return '';
  }

  return new Date(value).toLocaleString();
}

function App() {
  const [form, setForm] = useState(initialForm);
  const [fieldErrors, setFieldErrors] = useState({});
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setFieldErrors((prev) => ({ ...prev, [name]: undefined }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    const validationErrors = validateForm(form);
    setFieldErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setLoading(true);
    setResult(null);
    setError(null);

    try {
      const response = await fetch(`${API_BASE}/api/orders`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          orderNumber: form.orderNumber.trim(),
          userId: form.userId.trim(),
          payableAmount: Number(form.payableAmount),
          paymentGatewayId: form.paymentGatewayId,
          description: form.description.trim() || null,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        setError(data);
        return;
      }

      setResult(data);
    } catch (err) {
      setError({
        message: err.message ?? 'Failed to reach the billing API.',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app">
      <header>
        <h1>XYZ Inc. Billing</h1>
        <p>Submit a new order.</p>
      </header>

      <form className="order-form" onSubmit={handleSubmit} noValidate>
        <label>
          Order number
          <input
            name="orderNumber"
            value={form.orderNumber}
            onChange={handleChange}
            placeholder="ORD-1001"
          />
          {fieldErrors.orderNumber && (
            <span className="field-error">{fieldErrors.orderNumber}</span>
          )}
        </label>

        <label>
          User ID
          <input
            name="userId"
            value={form.userId}
            onChange={handleChange}
            placeholder="user-123"
          />
          {fieldErrors.userId && (
            <span className="field-error">{fieldErrors.userId}</span>
          )}
        </label>

        <label>
          Payable amount
          <input
            name="payableAmount"
            type="number"
            min="0.01"
            step="0.01"
            value={form.payableAmount}
            onChange={handleChange}
            placeholder="10.00"
          />
          {fieldErrors.payableAmount && (
            <span className="field-error">{fieldErrors.payableAmount}</span>
          )}
        </label>

        <label>
          Payment gateway
          <select
            name="paymentGatewayId"
            value={form.paymentGatewayId}
            onChange={handleChange}
          >
            <option value="stripe">stripe</option>
            <option value="paypal">paypal</option>
          </select>
        </label>

        <label>
          Description
          <input
            name="description"
            value={form.description}
            onChange={handleChange}
            placeholder="Monthly subscription"
          />
        </label>

        <button type="submit" disabled={loading}>
          {loading ? 'Submitting…' : 'Submit order'}
        </button>
      </form>

      {result && (
        <section className="panel success">
          <h2>Payment receipt</h2>
          <dl className="receipt">
            <dt>Order number</dt>
            <dd>{result.orderNumber}</dd>
            <dt>Amount</dt>
            <dd>${Number(result.amount).toFixed(2)}</dd>
            <dt>Timestamp</dt>
            <dd>{formatTimestamp(result.timestamp)}</dd>
            <dt>Confirmation</dt>
            <dd>{result.paymentConfirmation}</dd>
          </dl>
        </section>
      )}

      {error && (
        <section className="panel error">
          <h2>Payment error</h2>
          <p>{error.message}</p>
          {error.orderNumber && <p>Order: {error.orderNumber}</p>}
          {Array.isArray(error.details) && error.details.length > 0 && (
            <ul>
              {error.details.map((detail) => (
                <li key={detail}>{detail}</li>
              ))}
            </ul>
          )}
        </section>
      )}
    </div>
  );
}

export default App;
