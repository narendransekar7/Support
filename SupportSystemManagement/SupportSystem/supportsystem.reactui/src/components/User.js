import { useState, React } from "react";
import API from '../api/axios';
import logger from "../telemetry/logger";


function User() {
	
  
  const [formData,setFormData]= useState({
	  
	  FirstName:'Narendran',
	  LastName:'Sekar',
	  Country:'India',
	  Gender:'Male',
	  PrimaryEmail:'naren@gmail.com',
	  Password:'Naren@123',
	  PrimaryNumber:'1234567890',
	  Role:0
  });
  
  // Outcome of the last submit, shown above the button: { ok, text } or null.
  const [result, setResult] = useState(null);

  const handleSubmit = async (e) => {

	e.preventDefault(); // Prevent the default form submission
	setResult(null);

    // Send POST request to .NET Core Web API using Axios
    try {
		debugger;
      const response = await API.post('/user/createuser', formData);

      if (response.status === 200) {
        logger.info("User added");
        setResult({ ok: true, text: "User created." });
      } else {
        logger.warn("Adding user returned an unexpected status", { status: response.status });
        setResult({ ok: false, text: "The user couldn't be created." });
      }
    } catch (error) {
      const status = error.response?.status;
      // 403: createuser is Admin-only (SS.User.API checks the role in the session token).
      if (status === 403) {
        logger.warn("Adding user refused: not an administrator");
        setResult({ ok: false, text: "Only administrators can add users." });
      } else {
        logger.error("Adding user failed", error);
        setResult({ ok: false, text: "The user couldn't be created. Please try again." });
      }
    }
	  
  };
  
  const handleChange = (e) => {
	
	  
	 const { name, value } = e.target;
    setFormData({
		...formData,
      [name]: value,
    });
	  
  };
  
 
  
	
  return (
    <form onSubmit={handleSubmit} style={styles.form}>
      <div style={styles.inputContainer}>
        <label htmlFor="FirstName" style={styles.label}>First Name:</label>
        <input
          type="text"
          id="FirstName"
          name="FirstName"
		  value={formData.FirstName}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      <div style={styles.inputContainer}>
        <label htmlFor="LastName" style={styles.label}>Last Name:</label>
        <input
          type="text"
          id="LastName"
          name="LastName"
		  value={formData.LastName}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      <div style={styles.inputContainer}>
        <label htmlFor="Country" style={styles.label}>Country:</label>
        <input
          type="text"
          id="Country"
          name="Country"
		  value={formData.Country}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      <div style={styles.inputContainer}>
        <label htmlFor="Gender" style={styles.label}>Gender:</label>
        <input
          type="text"
          id="Gender"
          name="Gender"
		  value={formData.Gender}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      <div style={styles.inputContainer}>
        <label htmlFor="PrimaryEmail" style={styles.label}>Primary Email:</label>
        <input
          type="email"
          id="PrimaryEmail"
          name="PrimaryEmail"
		  value={formData.PrimaryEmail}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>
	  
	  <div style={styles.inputContainer}>
        <label htmlFor="Password" style={styles.label}>Password:</label>
        <input
          type="password"
          id="Password"
          name="Password"
		  value={formData.Password}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      <div style={styles.inputContainer}>
        <label htmlFor="PrimaryNumber" style={styles.label}>Primary Number:</label>
        <input
          type="text"
          id="PrimaryNumber"
          name="PrimaryNumber"
		  value={formData.PrimaryNumber}
		  onChange={handleChange}
          required
          style={styles.input}
        />
      </div>

      {result && (
        <p role={result.ok ? "status" : "alert"} style={{ ...styles.result, color: result.ok ? "#15803d" : "#dc2626" }}>
          {result.text}
        </p>
      )}
      <button type="submit" style={styles.button}>Submit</button>
    </form>
  );
}

// Style object for the form elements
const styles = {
  form: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    padding: '20px',
    backgroundColor: '#f7f7f7',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px',
    width: '400px',
    margin: '0 auto',
  },
  inputContainer: {
    marginBottom: '15px',
    width: '100%',
  },
  result: {
    marginTop: 0,
    marginBottom: '15px',
  },
  label: {
    display: 'block',
    marginBottom: '5px',
    fontWeight: 'bold',
    fontSize: '14px',
  },
  input: {
    width: '100%',
    padding: '10px',
    borderRadius: '4px',
    border: '1px solid #ccc',
    fontSize: '14px',
  },
  button: {
    padding: '10px 20px',
    backgroundColor: '#007bff',
    color: '#fff',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '16px',
    marginTop: '10px',
  },
};

export default User;
