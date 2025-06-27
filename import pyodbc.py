import pyodbc
from zk import ZK
from datetime import datetime
import urllib3
import time

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

zk = ZK('10.10.21.150', port=4370, timeout=825, password="1515")

CONNECTION_STRING = (
1234
)

def get_existing_log(cursor, user_id, timestamp):
    try:
        query = """
        SELECT COUNT(*) 
        FROM AttendanceLogs
        WHERE UserId = ? AND Timestamp = ?
        """
        cursor.execute(query, (user_id, timestamp))
        count = cursor.fetchone()[0]
        return count > 0
    except pyodbc.Error as e:
        print(f"❌ Error checking existing log: {e}")
        return False

def get_last_status(cursor, user_id, date):
    try:
        query = """
        SELECT TOP 1 Status
        FROM AttendanceLogs
        WHERE UserId = ? AND CAST(Timestamp AS DATE) = ?
        ORDER BY Timestamp DESC
        """
        cursor.execute(query, (user_id, date))
        result = cursor.fetchone()
        return result[0] if result else None
    except pyodbc.Error as e:
        print(f"❌ Error fetching last status: {e}")
        return None

def create_log(cursor, data):
    try:
        query = """
        INSERT INTO AttendanceLogs (UserId, Name, Timestamp, Status)
        VALUES (?, ?, ?, ?)
        """
        cursor.execute(query, (
            data['userId'],
            data['name'],
            data['timestamp'],
            data['status']
        ))
        cursor.connection.commit()
        print(f"➕ Created log for user {data['userId']} at {data['timestamp']} as {data['status']}")
        return True
    except pyodbc.Error as e:
        print(f"❌ Failed to create log: {e}")
        return False

def send_all_logs(conn, cursor, user_map):
    print("📤 Sending all existing logs...")
    logs = conn.get_attendance()
    logs = sorted(logs, key=lambda x: x.timestamp)
    processed_count = 0
    skipped_count = 0

    print("📦 Preloading existing logs from DB...")
    try:
        cursor.execute("SELECT UserId, Timestamp FROM AttendanceLogs")
        existing_set = set((str(row.UserId), row.Timestamp.isoformat()) for row in cursor.fetchall())
    except Exception as e:
        print(f"❌ Failed to preload logs: {e}")
        existing_set = set()

    for log in logs:
        user_id = str(log.user_id)
        timestamp = log.timestamp
        timestamp_str = timestamp.isoformat()
        date_str = timestamp.date().isoformat()
        name = user_map.get(log.user_id, "Unknown")

        if (user_id, timestamp_str) in existing_set:
            skipped_count += 1
            continue

        last_status = get_last_status(cursor, user_id, date_str)
        action = "Clock-In" if last_status in (None, "Clock-Out") else "Clock-Out"

        data = {
            "userId": user_id,
            "name": name,
            "timestamp": timestamp_str,
            "status": action
        }

        if create_log(cursor, data):
            processed_count += 1

    print(f"✅ Logs processed: {processed_count} inserted, {skipped_count} skipped")

def run_once():
    db_conn = None
    cursor = None
    device_conn = None
    try:
        db_conn = pyodbc.connect(CONNECTION_STRING)
        db_conn.autocommit = False
        cursor = db_conn.cursor()
        cursor.fast_executemany = True

        device_conn = zk.connect()
        device_conn.disable_device()

        users = device_conn.get_users()
        user_map = {user.user_id: user.name for user in users}

        send_all_logs(device_conn, cursor, user_map)

    except Exception as e:
        print(f"❌ Error occurred: {e}")
        raise e  # Let outer loop handle the retry

    finally:
        try:
            if cursor:
                cursor.close()
            if db_conn:
                db_conn.close()
            if device_conn:
                device_conn.enable_device()
                device_conn.disconnect()
        except Exception as e:
            print(f"❌ Cleanup error: {e}")

def main():
    print("🚀 Starting Attendance Sync Service (24/7 mode)")
    while True:
        try:
            run_once()
        except Exception:
            print("⏳ Waiting 5 seconds before retry...")
            time.sleep(5)

if __name__ == "__main__":
    main()
