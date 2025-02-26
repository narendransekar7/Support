import React, { useState } from "react";
import axios from 'axios';
import { useLoaderData } from "react-router-dom";

const TicketDetails = () => {
  const data = useLoaderData();

  const [ticket, setTicket] = useState({
    title: data.title,
    priority: data.priority,
    visibility: data.visibility,
    createdBy: data.createdBy,
    assignedTo: data.assignedTo
  });

	const [updates, setUpdates] = useState(
		data.updates?.map(update => ({
		user: update.updatedBy,
		content: update.content,
		timestamp: update.updatedAt
	})) || []
	);

  const [newUpdate, setNewUpdate] = useState("");

  const handleDropdownChange = (field, value) => {
    setTicket((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleAddUpdate = async (e) => {
    e.preventDefault();
    if (newUpdate.trim() === "") return;

    const newUpdateEntry = {
      user: data.createdBy, // need to be replaced with currently logged in user who is updating the ticket.
      content: newUpdate,
      timestamp: new Date().toLocaleString()
    };

	const newUpdateContent = {
      TicketId: data.ticketId, 
      Content: newUpdate,
      UpdatedBy: data.createdBy // need to be replaced with currently logged in user who is updating the ticket.,
    };

	try {
		debugger;
      const response = await axios.post('http://localhost:5145/api/ticketupdate/add', newUpdateContent, {
        headers: {
          'Content-Type': 'application/json'
		  ,Authorization: `Bearer ${localStorage.getItem('token')}`
        },
      });

      if (response.status === 200) {
        console.log('Ticket updated successfully', response.data);
        // Handle success, clear form, display message, etc.
      } else {
        console.error('Error while submitting', response.statusText);
      }
    } catch (error) {
      console.error('Error occurred during the request:', error.message);
    }

	setUpdates(updates => [...updates, newUpdateEntry]);
    //setUpdates([newUpdateEntry, ...updates]);
    setNewUpdate("");
  };

  return (
    <div style={styles.container}>
      <h2 style={styles.header}>Ticket Details</h2>
      <div style={styles.card}>
        <h3 style={styles.cardHeader}>{ticket.title}</h3>
        <div style={styles.row}>
          <div style={styles.field}>
            <label><strong>Priority:</strong></label>
            <select
              value={ticket.priority}
              onChange={(e) => handleDropdownChange("priority", e.target.value)}
              style={styles.dropdown}
            >
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </div>
          <div style={styles.field}>
            <label><strong>Visibility:</strong></label>
            <select
              value={ticket.visibility}
              onChange={(e) => handleDropdownChange("visibility", e.target.value)}
              style={styles.dropdown}
            >
              <option value="Public">Public</option>
              <option value="Internal">Internal</option>
            </select>
          </div>
        </div>
        <div style={styles.field}>
          <label><strong>Assigned To:</strong></label>
          <select
            value={ticket.assignedTo}
            onChange={(e) => handleDropdownChange("assignedTo", e.target.value)}
            style={styles.dropdown}
          >
            <option value="Jane Smith">Jane Smith</option>
            <option value="John Doe">John Doe</option>
            <option value="Alex Johnson">Alex Johnson</option>
          </select>
        </div>
        <p style={styles.field}>
          <strong>Created By:</strong> {ticket.createdBy}
        </p>
      </div>
      <h3 style={styles.subHeader}>Add Update</h3>
      <form onSubmit={handleAddUpdate} style={styles.form}>
        <textarea
          value={newUpdate}
          onChange={(e) => setNewUpdate(e.target.value)}
          placeholder="Type your update here..."
          rows="4"
          style={styles.textarea}
          required
        ></textarea>
        <button type="submit" style={styles.button}>
          Add Update
        </button>
      </form>

      <h3 style={styles.subHeader}>Previous Updates</h3>
      <div style={styles.updatesContainer}>
        {updates.length > 0 ? (
          updates.slice().reverse().map((update, index) => (
            <div key={index} style={styles.updateCard}>
              <p style={styles.updateHeader}>
                <strong>{update.user}</strong>{" "}
                <small style={styles.timestamp}>({update.timestamp})</small>
              </p>
              <p style={styles.updateContent}>{update.content}</p>
            </div>
          ))
        ) : (
          <p>No updates yet.</p>
        )}
      </div>
    </div>
  );
};

// Inline styles
const styles = {
  container: {
    fontFamily: "Arial, sans-serif",
    maxWidth: "800px",
    margin: "20px auto",
    padding: "20px",
    backgroundColor: "#f9f9f9",
    borderRadius: "8px",
    boxShadow: "0 2px 5px rgba(0, 0, 0, 0.1)",
  },
  header: {
    fontSize: "24px",
    color: "#333",
    marginBottom: "20px",
  },
  subHeader: {
    fontSize: "20px",
    color: "#555",
    marginTop: "30px",
    marginBottom: "10px",
  },
  card: {
    backgroundColor: "#fff",
    padding: "20px",
    borderRadius: "8px",
    boxShadow: "0 1px 3px rgba(0, 0, 0, 0.1)",
    marginBottom: "20px",
  },
  cardHeader: {
    fontSize: "18px",
    color: "#007bff",
    marginBottom: "10px",
  },
  row: {
    display: "flex",
    justifyContent: "space-between",
    marginBottom: "8px",
  },
  field: {
    fontSize: "14px",
    marginBottom: "10px",
  },
  dropdown: {
    marginLeft: "10px",
    padding: "5px",
    borderRadius: "4px",
    border: "1px solid #ccc",
    fontSize: "14px",
  },
  content: {
    marginTop: "10px",
    fontSize: "15px",
    lineHeight: "1.5",
    color: "#444",
  },
  form: {
    marginBottom: "20px",
  },
  textarea: {
    width: "100%",
    padding: "10px",
    borderRadius: "4px",
    border: "1px solid #ccc",
    marginBottom: "10px",
    fontSize: "14px",
  },
  button: {
    padding: "10px 20px",
    backgroundColor: "#007bff",
    color: "#fff",
    border: "none",
    borderRadius: "4px",
    cursor: "pointer",
  },
  updatesContainer: {
    borderTop: "1px solid #ddd",
    paddingTop: "10px",
  },
  updateCard: {
    backgroundColor: "#fff",
    padding: "15px",
    borderRadius: "6px",
    boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
    marginBottom: "10px",
  },
  updateHeader: {
    marginBottom: "6px",
    fontSize: "14px",
    color: "#007bff",
  },
  timestamp: {
    color: "#999",
    fontSize: "12px",
  },
  updateContent: {
    fontSize: "14px",
    color: "#333",
  },
};

export default TicketDetails;
