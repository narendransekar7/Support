// SS.User.API sends Role as the enum's number (SS.Base.Domain.Entities.Role: Admin, Agent, Customer).
export const ROLE_NAMES = { 0: "Admin", 1: "Agent", 2: "Customer" };

export const ADMIN_ROLE = 0;

export const isAdmin = (role) => role === ADMIN_ROLE;

// Where a signed-in user lands (after login, or when a page isn't theirs): admins manage users, everyone
// else starts with tickets.
export const homePathFor = (role) => (isAdmin(role) ? "/user/add" : "/ticket/create");
