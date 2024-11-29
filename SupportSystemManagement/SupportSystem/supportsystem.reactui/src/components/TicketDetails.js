import React, { useState } from "react";

const TicketDetails = () => {
  // Sample ticket data
  const [ticket, setTicket] = useState({
    title: "Sample Ticket",
    priority: "Medium",
    visibility: "Public",
    createdBy: "John Doe",
    assignedTo: "Jane Smith",
    content: "This is the initial ticket content.",
  });

  // Sample updates data
  const [updates, setUpdates] = useState([
    { user: "John Doe", content: "Initial update.", timestamp: "2024-11-24 10:00 AM" },
    { user: "Jane Smith", content: "Acknowledged.", timestamp: "2024-11-24 11:00 AM" },
  ]);

  // State for new update
  const [newUpdate, setNewUpdate] = useState("");

  // Handle dropdown change
  const handleDropdownChange = (field, value) => {
    setTicket((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  // Handle adding a new update
  const handleAddUpdate = (e) => {
    e.preventDefault();
    if (newUpdate.trim() === "") return;

    const newUpdateEntry = {
      user: "Current User", // Replace with dynamic user data
      content: newUpdate,
      timestamp: new Date().toLocaleString(),
    };

    setUpdates([newUpdateEntry, ...updates]);
    setNewUpdate("");
  };

  return (
    <div style={styles.container}>
      {/* Ticket Details */}
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
        <p style={styles.content}>
          <strong>Content:</strong> {ticket.content}
        </p>
      </div>

      {/* Add Update */}
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

      {/* Previous Updates */}
      <h3 style={styles.subHeader}>Previous Updates</h3>
      <div style={styles.updatesContainer}>
        {updates.length > 0 ? (
          updates.map((update, index) => (
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
