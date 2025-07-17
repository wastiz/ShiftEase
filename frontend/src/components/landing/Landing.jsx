import { useNavigate } from 'react-router-dom';
import './Landing.css';

function Landing() {
  const navigate = useNavigate();

  const handleStart = () => {
    navigate('/login');
  };

  return (
    <div className="landing-container">
      <section className="hero-section">
        <div className="hero-content">
          <h1>ShiftEase</h1>
          <h2>Shift Management Made Simple</h2>
          <p>
            ShiftEase is the perfect solution for small teams. Create, manage, and optimize shifts in just a few clicks.
          </p>
          <button className="btn-primary" onClick={handleStart}>
            Start Now
          </button>
        </div>
      </section>

      <section className="features-section">
        <div className="feature">
          <h3>Automatic Shift Scheduling</h3>
          <p>
            Our smart algorithm automatically assigns shifts based on employee preferences and business needs.
          </p>
        </div>
        <div className="feature">
          <h3>Simple and Intuitive</h3>
          <p>
            An easy-to-use interface allows you to set up schedules quickly and effortlessly.
          </p>
        </div>
        <div className="feature">
          <h3>Built for Small Teams</h3>
          <p>
            ShiftEase is designed for small businesses where every team member counts.
          </p>
        </div>
      </section>

      <section className="cta-section">
        <h2>Try ShiftEase Today</h2>
        <p>
          Start for free and see how easy it is to manage shifts with ShiftEase.
        </p>
        <button className="btn-primary" onClick={handleStart}>
          Get Started
        </button>
      </section>
    </div>
  );
}

export default Landing;