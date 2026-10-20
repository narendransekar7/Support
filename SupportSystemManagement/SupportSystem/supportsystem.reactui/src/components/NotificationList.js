import React from "react";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { markNotificationRead } from "../features/notificationsSlice";

// Icon per notification type (SS.Base.Domain NotificationType).
const typeIcons = {
  TicketCreated: "🎫",
  Welcome: "👋",
};

// "5m ago", "3h ago", "2d ago" - falls back to the date after a week.
export const timeAgo = (value) => {
  // The API sends UTC with a "Z", so the browser converts it to the user's local time.
  const date = new Date(value);
  const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
  if (Number.isNaN(seconds)) return "";
  if (seconds < 60) return "just now";
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
  if (seconds < 604800) return `${Math.floor(seconds / 86400)}d ago`;
  return date.toLocaleDateString();
};

// The notification rows, shared by the header bell's dropdown and the Notifications page.
// Clicking a row marks it read and, for a ticket notification, opens the ticket.
const NotificationList = ({ items, onNavigate, emptyText = "You're all caught up." }) => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  if (!items.length) {
    return <p style={styles.empty}>{emptyText}</p>;
  }

  const open = (notification) => {
    if (!notification.isRead) dispatch(markNotificationRead(notification.notificationId));
    if (notification.ticketId) {
      navigate(`/ticket/${notification.ticketId}`);
      onNavigate?.();
    }
  };

  return (
    <ul style={styles.list}>
      {items.map((n) => (
        <li key={n.notificationId}>
          <button
            type="button"
            onClick={() => open(n)}
            style={{ ...styles.row, backgroundColor: n.isRead ? "#ffffff" : "#eff6ff" }}
            title={n.ticketId ? "Open the ticket" : n.isRead ? "" : "Mark as read"}
          >
            <span style={styles.icon} aria-hidden="true">{typeIcons[n.type] ?? "🔔"}</span>
            <span style={styles.body}>
              <span style={{ ...styles.message, fontWeight: n.isRead ? "normal" : "bold" }}>{n.message}</span>
              <span style={styles.time}>{timeAgo(n.createdAt)}</span>
            </span>
            {!n.isRead && <span style={styles.unreadDot} aria-label="Unread" />}
          </button>
        </li>
      ))}
    </ul>
  );
};

const styles = {
  list: { listStyle: "none", margin: 0, padding: 0 },
  row: {
    display: "flex",
    alignItems: "flex-start",
    gap: "10px",
    width: "100%",
    padding: "10px 12px",
    border: "none",
    borderBottom: "1px solid #e2e8f0",
    textAlign: "left",
    cursor: "pointer",
    fontFamily: "inherit",
  },
  icon: { fontSize: "18px", lineHeight: "20px" },
  body: { flex: 1, display: "flex", flexDirection: "column", gap: "2px" },
  message: { fontSize: "14px", color: "#1e293b" },
  time: { fontSize: "12px", color: "#64748b" },
  unreadDot: { width: "8px", height: "8px", borderRadius: "50%", backgroundColor: "#3b82f6", marginTop: "6px" },
  empty: { padding: "16px", margin: 0, color: "#64748b", fontSize: "14px", textAlign: "center" },
};

export default NotificationList;
