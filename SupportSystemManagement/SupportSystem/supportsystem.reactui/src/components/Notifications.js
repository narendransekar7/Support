import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchNotifications, markAllNotificationsRead } from "../features/notificationsSlice";
import NotificationList from "./NotificationList";

// /notifications - the full list (newest 100), with "mark all as read".
const Notifications = () => {
  const dispatch = useDispatch();
  const { items, unreadCount, status } = useSelector((state) => state.notifications);

  useEffect(() => {
    dispatch(fetchNotifications(100));
  }, [dispatch]);

  return (
    <div style={styles.card}>
      <div style={styles.header}>
        <h2 style={styles.title}>
          Notifications{unreadCount > 0 && <span style={styles.count}>{unreadCount} unread</span>}
        </h2>
        <button
          type="button"
          onClick={() => dispatch(markAllNotificationsRead())}
          disabled={unreadCount === 0}
          style={{ ...styles.button, opacity: unreadCount === 0 ? 0.5 : 1 }}
        >
          Mark all as read
        </button>
      </div>
      {status === "loading" ? (
        <p style={styles.info}>Loading…</p>
      ) : status === "failed" && items.length === 0 ? (
        <p style={styles.info}>Notifications couldn't be loaded. Try again in a moment.</p>
      ) : (
        <NotificationList items={items} emptyText="No notifications yet." />
      )}
    </div>
  );
};

const styles = {
  card: {
    maxWidth: "760px",
    margin: "0 auto",
    backgroundColor: "#ffffff",
    border: "1px solid #e2e8f0",
    borderRadius: "8px",
    overflow: "hidden",
  },
  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "14px 16px",
    borderBottom: "1px solid #e2e8f0",
    gap: "12px",
    flexWrap: "wrap",
  },
  title: { margin: 0, fontSize: "18px", color: "#1e293b", display: "flex", alignItems: "center", gap: "10px" },
  count: {
    fontSize: "12px",
    fontWeight: "normal",
    color: "#1d4ed8",
    backgroundColor: "#dbeafe",
    borderRadius: "10px",
    padding: "2px 8px",
  },
  button: {
    backgroundColor: "#3b82f6",
    color: "#fff",
    border: "none",
    padding: "6px 12px",
    borderRadius: "4px",
    cursor: "pointer",
  },
  info: { padding: "16px", margin: 0, color: "#64748b" },
};

export default Notifications;
