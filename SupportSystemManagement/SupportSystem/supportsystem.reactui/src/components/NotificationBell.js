import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Link } from "react-router-dom";
import { fetchNotifications, markAllNotificationsRead } from "../features/notificationsSlice";
import NotificationList from "./NotificationList";

// How often the bell refreshes while the app is open. Notifications are written by SS.Notification.API's
// message consumers a moment after the action (ticket/user created), so a short poll picks them up.
const POLL_MS = 30000;

// Header bell: unread badge + a dropdown with the newest notifications.
const NotificationBell = () => {
  const dispatch = useDispatch();
  const { items, unreadCount } = useSelector((state) => state.notifications);
  const [open, setOpen] = useState(false);
  const containerRef = useRef(null);

  // Load now, then poll - but not while the tab is in the background.
  useEffect(() => {
    dispatch(fetchNotifications());
    const timer = setInterval(() => {
      if (document.visibilityState === "visible") dispatch(fetchNotifications());
    }, POLL_MS);
    return () => clearInterval(timer);
  }, [dispatch]);

  // Close the dropdown on an outside click or Escape.
  useEffect(() => {
    if (!open) return undefined;
    const onClick = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target)) setOpen(false);
    };
    const onKey = (e) => e.key === "Escape" && setOpen(false);
    document.addEventListener("mousedown", onClick);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onClick);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const toggle = () => {
    if (!open) dispatch(fetchNotifications());
    setOpen(!open);
  };

  return (
    <div ref={containerRef} style={styles.container}>
      <button
        type="button"
        onClick={toggle}
        style={styles.bell}
        aria-label={unreadCount ? `Notifications, ${unreadCount} unread` : "Notifications"}
        aria-expanded={open}
      >
        <span aria-hidden="true">🔔</span>
        {unreadCount > 0 && <span style={styles.badge}>{unreadCount > 99 ? "99+" : unreadCount}</span>}
      </button>

      {open && (
        <div style={styles.panel} role="dialog" aria-label="Notifications">
          <div style={styles.panelHeader}>
            <span style={styles.panelTitle}>Notifications</span>
            <button
              type="button"
              onClick={() => dispatch(markAllNotificationsRead())}
              disabled={unreadCount === 0}
              style={{ ...styles.linkButton, opacity: unreadCount === 0 ? 0.5 : 1 }}
            >
              Mark all as read
            </button>
          </div>
          <div style={styles.panelBody}>
            <NotificationList items={items.slice(0, 8)} onNavigate={() => setOpen(false)} />
          </div>
          <Link to="/notifications" onClick={() => setOpen(false)} style={styles.viewAll}>
            View all notifications
          </Link>
        </div>
      )}
    </div>
  );
};

const styles = {
  container: { position: "relative", marginRight: "12px" },
  bell: {
    position: "relative",
    background: "none",
    border: "1px solid #e2e8f0",
    borderRadius: "6px",
    padding: "4px 10px",
    fontSize: "18px",
    cursor: "pointer",
  },
  badge: {
    position: "absolute",
    top: "-6px",
    right: "-6px",
    minWidth: "18px",
    height: "18px",
    padding: "0 5px",
    borderRadius: "9px",
    backgroundColor: "#ef4444",
    color: "#fff",
    fontSize: "11px",
    fontWeight: "bold",
    lineHeight: "18px",
    textAlign: "center",
  },
  panel: {
    position: "absolute",
    right: 0,
    top: "calc(100% + 8px)",
    width: "360px",
    maxWidth: "calc(100vw - 32px)",
    backgroundColor: "#ffffff",
    border: "1px solid #e2e8f0",
    borderRadius: "8px",
    boxShadow: "0 8px 24px rgba(15, 23, 42, 0.15)",
    zIndex: 1000,
    overflow: "hidden",
  },
  panelHeader: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "10px 12px",
    borderBottom: "1px solid #e2e8f0",
  },
  panelTitle: { fontWeight: "bold", color: "#1e293b" },
  panelBody: { maxHeight: "360px", overflowY: "auto" },
  linkButton: {
    background: "none",
    border: "none",
    color: "#3b82f6",
    cursor: "pointer",
    fontSize: "13px",
    padding: 0,
  },
  viewAll: {
    display: "block",
    padding: "10px",
    textAlign: "center",
    fontSize: "13px",
    color: "#3b82f6",
    textDecoration: "none",
    borderTop: "1px solid #e2e8f0",
  },
};

export default NotificationBell;
