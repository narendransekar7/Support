import { useState, React } from "react"; 
import axios from 'axios';


function User() {
	
  
  const [formData,setFormData]= useState({
	  
	  FirstName:'Narendran',
	  LastName:'Sekar',
	  Country:'India',
	  Gender:'Male',
	  PrimaryEmail:'naren@gmail.com',
	  PrimaryNumber:'1234567890'  
	  
  });
  
  const handleSubmit = async (e) => {
	  
	debugger;
	e.preventDefault(); // Prevent the default form submission

    // Send POST request to .NET Core Web API using Axios
    try {
      const response = await axios.post('https://localhost:4001/user/createagentuser', formData, {
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.status === 200) {
        console.log('User added successfully:', response.data);
        // Handle success, clear form, display message, etc.
      } else {
        console.error('Error submitting the form:', response.statusText);
      }
    } catch (error) {
      console.error('Error occurred during the request:', error.message);
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
