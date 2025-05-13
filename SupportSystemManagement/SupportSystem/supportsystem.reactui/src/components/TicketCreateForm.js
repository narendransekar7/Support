import React, { useState, useEffect  } from "react";
import axios from "axios";
import {useSelector } from "react-redux";


const CreateTicketForm = () => {
	
  const auth = useSelector((state) => state.auth);
	
	
  const [formData, setFormData] = useState({
    Title: "",
    Content: "",
    Priority: "Medium",
    Visibility: "Public",
    CreatedBy: auth.userid,
    AssignedTo: "C7826BAD-4B94-4A03-B068-D92B17234C65",
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
	//console.log(auth);
    setFormData({
      ...formData,
      [name]: value,
    });
  };
  
    // Update CreatedBy when auth.userid is available
  useEffect(() => {
    if (auth?.userid) {
      setFormData((prevState) => ({
        ...prevState,
        CreatedBy: auth.userid, // Set the userid once it is available
      }));
    }
  }, [auth]);  // This will run when `auth` changes

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post("http://localhost:5145/api/ticket/create", formData, {
        headers: {
          "Content-Type": "application/json",
		   Authorization: `Bearer ${localStorage.getItem('token')}`
        },
      });

      if (response.status === 200) {
        console.log("Ticket created successfully:", response.data);
        alert("Ticket Created!");
      } else {
        console.error("Error submitting the form:", response.statusText);
      }
    } catch (error) {
      console.error("Error occurred during the request:", error.message);
    }
  };

  return (
    <div style={styles.container}>
      <h2 style={styles.header}>Create Ticket</h2>
      <form onSubmit={handleSubmit} style={styles.form}>
        {/* Title */}
        <label style={styles.label}>Title</label>
        <input
          type="text"
          name="Title"
          value={formData.Title}
          onChange={handleChange}
          placeholder="Enter ticket title"
          style={styles.input}
          required
        />

        {/* Content */}
        <label style={styles.label}>Content</label>
        <textarea
          name="Content"
          value={formData.Content}
          onChange={handleChange}
          placeholder="Enter ticket content"
          rows="6"
          style={styles.textarea}
          required
        ></textarea>

        {/* Priority */}
        <label style={styles.label}>Priority</label>
        <select
          name="Priority"
          value={formData.Priority}
          onChange={handleChange}
          style={styles.select}
        >
          <option value="High">High</option>
          <option value="Medium">Medium</option>
          <option value="Low">Low</option>
        </select>

        {/* Visibility */}
        <label style={styles.label}>Visibility</label>
        <select
          name="Visibility"
          value={formData.Visibility}
          onChange={handleChange}
          style={styles.select}
        >
          <option value="Public">Public</option>
          <option value="Internal">Internal</option>
        </select>

        {/* Submit Button */}
        <button type="submit" style={styles.button}>
          Create Ticket
        </button>
      </form>
    </div>
  );
};

// Inline styles
const styles = {
  container: {
    maxWidth: "600px",
    margin: "20px auto",
    padding: "20px",
    backgroundColor: "#f9f9f9",
    borderRadius: "8px",
    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)",
    fontFamily: "Arial, sans-serif",
  },
  header: {
    fontSize: "24px",
    textAlign: "center",
    marginBottom: "20px",
    color: "#333",
  },
  form: {
    display: "flex",
    flexDirection: "column",
  },
  label: {
    marginBottom: "8px",
    fontWeight: "bold",
    fontSize: "14px",
    color: "#555",
  },
  input: {
    padding: "10px",
    marginBottom: "16px",
    border: "1px solid #ddd",
    borderRadius: "4px",
    fontSize: "14px",
    outline: "none",
    transition: "border-color 0.3s",
  },
  textarea: {
    padding: "10px",
    marginBottom: "16px",
    border: "1px solid #ddd",
    borderRadius: "4px",
    fontSize: "14px",
    resize: "vertical",
    outline: "none",
    transition: "border-color 0.3s",
  },
  select: {
    padding: "10px",
    marginBottom: "16px",
    border: "1px solid #ddd",
    borderRadius: "4px",
    fontSize: "14px",
    outline: "none",
    transition: "border-color 0.3s",
  },
  button: {
    padding: "12px",
    backgroundColor: "#007bff",
    color: "white",
    border: "none",
    borderRadius: "4px",
    cursor: "pointer",
    fontSize: "16px",
    fontWeight: "bold",
    transition: "background-color 0.3s",
  },
  buttonHover: {
    backgroundColor: "#0056b3",
  },
};

export default CreateTicketForm;
